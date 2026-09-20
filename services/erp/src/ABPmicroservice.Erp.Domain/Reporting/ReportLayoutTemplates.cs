namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// The layout every report prints through until someone customises it.
/// <para>
/// Business Central ships a built-in layout per report and lets a user download it as the starting
/// point for their own. This is that starting point: it uses every placeholder the language has,
/// so a user editing a copy can see what is available without reading documentation.
/// </para>
/// </summary>
public static class ReportLayoutTemplates
{
    public const string BuiltIn = """
        <!DOCTYPE html>
        <html>
        <head>
        <meta charset="utf-8">
        <title>{{Title}}</title>
        <style>
          body { font-family: Segoe UI, Helvetica, Arial, sans-serif; color: #212529; margin: 2rem; }
          header { border-bottom: 2px solid #212529; padding-bottom: .75rem; margin-bottom: 1.25rem; }
          .company { font-size: 1.35rem; font-weight: 600; }
          h1 { font-size: 1.1rem; font-weight: 600; margin: .35rem 0 0; }
          .period { color: #6c757d; font-size: .85rem; margin-top: .2rem; }
          table { width: 100%; border-collapse: collapse; font-size: .85rem; }
          th { text-align: left; border-bottom: 1px solid #adb5bd; padding: .4rem .5rem; white-space: nowrap; }
          td { padding: .3rem .5rem; border-bottom: 1px solid #f1f3f5; }
          td.number { text-align: right; font-variant-numeric: tabular-nums; }
          td.date { white-space: nowrap; }
          tr.total td { font-weight: 600; border-top: 1px solid #adb5bd; background: #f8f9fa; }
          tr.reversed td { font-style: italic; color: #6c757d; }
          tr.indent-1 td:first-child { padding-left: 1.5rem; }
          tr.indent-2 td:first-child { padding-left: 3rem; }
          tr.indent-3 td:first-child { padding-left: 4.5rem; }
          tr.indent-4 td:first-child { padding-left: 6rem; }
          tr.indent-5 td:first-child { padding-left: 7.5rem; }
          footer { margin-top: 1.5rem; color: #6c757d; font-size: .75rem; }
          @media print { body { margin: 0; } footer { position: fixed; bottom: 0; } }
        </style>
        </head>
        <body>
        <header>
          <div class="company">{{CompanyName}}</div>
          <h1>{{Title}}</h1>
          <div class="period">{{FromDate}} &ndash; {{ToDate}}</div>
        </header>
        <table>
          <thead>
            <tr>{{#Columns}}<th>{{Header}}</th>{{/Columns}}</tr>
          </thead>
          <tbody>
            {{#Rows}}<tr class="{{RowClass}} indent-{{Indent}}">{{#Cells}}<td class="{{Kind}}">{{Value}}</td>{{/Cells}}</tr>
            {{/Rows}}
          </tbody>
        </table>
        <footer>Printed {{PrintedOn}}</footer>
        </body>
        </html>
        """;
}
