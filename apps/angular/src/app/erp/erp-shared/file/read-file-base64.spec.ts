import { readFileBase64, readUploadFile, stripDataUrlPrefix } from './read-file-base64';

describe('stripDataUrlPrefix', () => {
  it('keeps only the base64 part of a data URL', () => {
    expect(stripDataUrlPrefix('data:text/csv;base64,YSxiCjEsMg==')).toBe('YSxiCjEsMg==');
  });

  it('handles a data URL without a media type', () => {
    expect(stripDataUrlPrefix('data:;base64,QUJD')).toBe('QUJD');
  });

  it('returns an empty string for an empty file', () => {
    expect(stripDataUrlPrefix('data:')).toBe('');
    expect(stripDataUrlPrefix('data:application/octet-stream;base64,')).toBe('');
  });

  it('passes a value that is not a data URL through', () => {
    expect(stripDataUrlPrefix('QUJD')).toBe('QUJD');
    expect(stripDataUrlPrefix('')).toBe('');
  });
});

describe('readFileBase64', () => {
  it('reads a file into plain base64', done => {
    const file = new File(['a,b\n1,2'], 'customers.csv', { type: 'text/csv' });

    readFileBase64(file).subscribe(value => {
      expect(value).toBe(btoa('a,b\n1,2'));
      done();
    });
  });

  it('reads binary content byte for byte', done => {
    const bytes = new Uint8Array([0x50, 0x4b, 0x03, 0x04, 0xff, 0x00]);

    readFileBase64(new Blob([bytes])).subscribe(value => {
      const decoded = Uint8Array.from(atob(value), c => c.charCodeAt(0));
      expect(Array.from(decoded)).toEqual(Array.from(bytes));
      done();
    });
  });

  it('pairs the content with the file name for an upload', done => {
    const file = new File(['{}'], 'package.json', { type: 'application/json' });

    readUploadFile(file).subscribe(value => {
      expect(value).toEqual({ fileName: 'package.json', contentBase64: btoa('{}') });
      done();
    });
  });
});
