using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using Volo.Abp;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>What the report is being rendered for, beyond the figures themselves.</summary>
public sealed class ReportRenderContext
{
    public string CompanyName { get; set; }

    public DateTime PrintedOn { get; set; }
}

/// <summary>
/// The layout language of a report: literal markup with placeholders and repeated sections.
/// <para>
/// Business Central lets a user download a report's layout, edit it and upload it again. This is
/// the same idea with HTML in place of Word or RDLC. A general-purpose template engine would give
/// more expressive layouts, but layouts are uploaded by users and rendered on the server, so the
/// vocabulary is deliberately fixed and contains no way to execute anything: a template can place
/// and repeat the report's own values, and nothing else.
/// </para>
/// <para>
/// Placeholders are <c>{{Name}}</c> and sections are <c>{{#Name}}…{{/Name}}</c>. Every substituted
/// value is HTML-escaped, so the markup is the template author's and the data is only ever data.
/// </para>
/// </summary>
public sealed class ReportTemplate
{
    /// <summary>Values and sections a template may use, by the section it appears in.</summary>
    private static readonly Dictionary<string, Vocabulary> Vocabularies = new(StringComparer.OrdinalIgnoreCase)
    {
        [RootScope] = new(["Title", "FromDate", "ToDate", "CompanyName", "PrintedOn"], ["Columns", "Rows"]),
        ["Columns"] = new(["Key", "Header", "Kind"], []),
        ["Rows"] = new(["Indent", "RowClass"], ["Cells"]),
        ["Cells"] = new(["Key", "Header", "Kind", "Value"], []),
    };

    private const string RootScope = "";

    /// <summary>A named cell of the current row, e.g. <c>{{Cell:accountNo}}</c>.</summary>
    private const string CellPrefix = "Cell:";

    private readonly IReadOnlyList<Node> _nodes;

    private ReportTemplate(IReadOnlyList<Node> nodes)
    {
        _nodes = nodes;
    }

    /// <summary>
    /// Reads a template and checks it. Every name is checked against what its section actually
    /// offers, so a template that would have rendered blank is refused when it is saved instead.
    /// </summary>
    public static ReportTemplate Parse(string text)
    {
        if (text.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Reports.LayoutTemplateEmpty);
        }

        var root = new List<Node>();
        var open = new Stack<(string Name, List<Node> Nodes)>();
        open.Push((RootScope, root));

        var position = 0;

        while (position < text.Length)
        {
            var start = text.IndexOf("{{", position, StringComparison.Ordinal);
            if (start < 0)
            {
                Append(open.Peek().Nodes, text[position..]);
                break;
            }

            if (start > position)
            {
                Append(open.Peek().Nodes, text[position..start]);
            }

            var end = text.IndexOf("}}", start + 2, StringComparison.Ordinal);
            if (end < 0)
            {
                throw Invalid("UnclosedPlaceholder", text[start..Math.Min(text.Length, start + 20)]);
            }

            var token = text[(start + 2)..end].Trim();
            position = end + 2;

            if (token.Length == 0)
            {
                throw Invalid("EmptyPlaceholder", "{{}}");
            }

            if (token[0] == '#')
            {
                var name = token[1..].Trim();
                EnsureSectionExists(open.Peek().Name, name);
                open.Push((name, []));
            }
            else if (token[0] == '/')
            {
                var name = token[1..].Trim();
                if (open.Count == 1)
                {
                    throw Invalid("UnopenedSection", token);
                }

                var (openName, nodes) = open.Pop();
                if (!string.Equals(openName, name, StringComparison.OrdinalIgnoreCase))
                {
                    throw Invalid("MismatchedSection", $"{openName}/{name}");
                }

                open.Peek().Nodes.Add(new Node(NodeKind.Section, openName, nodes));
            }
            else
            {
                EnsureValueExists(open.Peek().Name, token);
                open.Peek().Nodes.Add(new Node(NodeKind.Value, token, null));
            }
        }

        if (open.Count > 1)
        {
            throw Invalid("UnclosedSection", open.Peek().Name);
        }

        return new ReportTemplate(root);
    }

    /// <summary>Reads a template only to check it, for validation on save.</summary>
    public static void Validate(string text)
    {
        Parse(text);
    }

    public string Render(ReportResult result, ReportRenderContext context)
    {
        Check.NotNull(result, nameof(result));

        var builder = new StringBuilder();
        Render(_nodes, new RootValues(result, context ?? new ReportRenderContext()), builder);

        return builder.ToString();
    }

    private static void Render(IEnumerable<Node> nodes, IScope scope, StringBuilder builder)
    {
        foreach (var node in nodes)
        {
            switch (node.Kind)
            {
                case NodeKind.Literal:
                    builder.Append(node.Name);
                    break;

                case NodeKind.Value:
                    builder.Append(WebUtility.HtmlEncode(scope.Value(node.Name) ?? string.Empty));
                    break;

                case NodeKind.Section:
                    foreach (var child in scope.Section(node.Name))
                    {
                        Render(node.Children, child, builder);
                    }

                    break;
            }
        }
    }

    private static void Append(List<Node> nodes, string literal)
    {
        if (literal.Length > 0)
        {
            nodes.Add(new Node(NodeKind.Literal, literal, null));
        }
    }

    private static void EnsureSectionExists(string scope, string name)
    {
        if (!Vocabularies.TryGetValue(scope, out var vocabulary) || !vocabulary.Sections.Contains(name))
        {
            throw Invalid("UnknownSection", name);
        }
    }

    private static void EnsureValueExists(string scope, string name)
    {
        // A named cell can be any of the report's own column keys, which differ per report, so
        // it is accepted here and resolves to nothing if that column is not in this report.
        if (scope.Equals("Rows", StringComparison.OrdinalIgnoreCase) && name.StartsWith(CellPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!Vocabularies.TryGetValue(scope, out var vocabulary) || !vocabulary.Values.Contains(name))
        {
            throw Invalid("UnknownValue", name);
        }
    }

    private static BusinessException Invalid(string reason, string token)
    {
        return new BusinessException(ErpErrorCodes.Reports.LayoutTemplateNotValid)
            .WithData("reason", reason)
            .WithData("token", token);
    }

    /// <summary>Formats one value the way its column says it should read.</summary>
    internal static string Format(object value, ReportColumnKind kind)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return kind switch
        {
            ReportColumnKind.Number => value is IFormattable number
                ? number.ToString("N2", CultureInfo.CurrentCulture)
                : value.ToString(),
            ReportColumnKind.Date => value is DateTime date ? date.ToString("d", CultureInfo.CurrentCulture) : value.ToString(),
            _ => value.ToString(),
        };
    }

    private sealed record Vocabulary(IReadOnlyCollection<string> ValueNames, IReadOnlyCollection<string> SectionNames)
    {
        public HashSet<string> Values { get; } = new(ValueNames, StringComparer.OrdinalIgnoreCase);

        public HashSet<string> Sections { get; } = new(SectionNames, StringComparer.OrdinalIgnoreCase);
    }

    private enum NodeKind
    {
        Literal,
        Value,
        Section,
    }

    private sealed record Node(NodeKind Kind, string Name, IReadOnlyList<Node> Children);

    /// <summary>One level of the template's data: what a placeholder resolves against.</summary>
    private interface IScope
    {
        string Value(string name);

        IEnumerable<IScope> Section(string name);
    }

    private sealed class RootValues : IScope
    {
        private readonly ReportResult _result;
        private readonly ReportRenderContext _context;

        public RootValues(ReportResult result, ReportRenderContext context)
        {
            _result = result;
            _context = context;
        }

        public string Value(string name)
        {
            return name.ToLowerInvariant() switch
            {
                "title" => _result.Title,
                "fromdate" => _result.FromDate.ToString("d", CultureInfo.CurrentCulture),
                "todate" => _result.ToDate.ToString("d", CultureInfo.CurrentCulture),
                "companyname" => _context.CompanyName,
                "printedon" => (_context.PrintedOn == default ? DateTime.Now : _context.PrintedOn).ToString(
                    "g",
                    CultureInfo.CurrentCulture
                ),
                _ => null,
            };
        }

        public IEnumerable<IScope> Section(string name)
        {
            return name.ToLowerInvariant() switch
            {
                "columns" => _result.Columns.Select(c => new ColumnValues(c)),
                "rows" => _result.Rows.Select(r => new RowValues(_result, r)),
                _ => [],
            };
        }
    }

    private sealed class ColumnValues : IScope
    {
        private readonly ReportColumnDefinition _column;

        public ColumnValues(ReportColumnDefinition column)
        {
            _column = column;
        }

        public string Value(string name)
        {
            return name.ToLowerInvariant() switch
            {
                "key" => _column.Key,
                "header" => _column.Header,
                "kind" => _column.Kind.ToString().ToLowerInvariant(),
                _ => null,
            };
        }

        public IEnumerable<IScope> Section(string name) => [];
    }

    private sealed class RowValues : IScope
    {
        private readonly ReportResult _result;
        private readonly ReportRow _row;

        public RowValues(ReportResult result, ReportRow row)
        {
            _result = result;
            _row = row;
        }

        public string Value(string name)
        {
            if (name.StartsWith(CellPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var key = name[CellPrefix.Length..];
                var column = _result.Columns.FirstOrDefault(c =>
                    string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase)
                );

                return column == null ? string.Empty : Format(_row.Values.GetOrDefault(column.Key), column.Kind);
            }

            return name.ToLowerInvariant() switch
            {
                "indent" => _row.Indentation.ToString(CultureInfo.InvariantCulture),
                // Presentation flags become class names so the styling stays in the template.
                "rowclass" => string.Join(' ', new[] { _row.Bold ? "total" : null, _row.Italic ? "reversed" : null }.Where(c => c != null)),
                _ => null,
            };
        }

        public IEnumerable<IScope> Section(string name)
        {
            return name.Equals("Cells", StringComparison.OrdinalIgnoreCase)
                ? _result.Columns.Select(c => new CellValues(c, _row.Values.GetOrDefault(c.Key)))
                : [];
        }
    }

    private sealed class CellValues : IScope
    {
        private readonly ReportColumnDefinition _column;
        private readonly object _value;

        public CellValues(ReportColumnDefinition column, object value)
        {
            _column = column;
            _value = value;
        }

        public string Value(string name)
        {
            return name.ToLowerInvariant() switch
            {
                "key" => _column.Key,
                "header" => _column.Header,
                "kind" => _column.Kind.ToString().ToLowerInvariant(),
                "value" => Format(_value, _column.Kind),
                _ => null,
            };
        }

        public IEnumerable<IScope> Section(string name) => [];
    }
}
