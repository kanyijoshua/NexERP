import { Observable } from 'rxjs';

/** A file as the ERP API takes it: its name and its bytes as plain base64 inside the JSON body. */
export interface Base64File {
  fileName: string;
  contentBase64: string;
}

/**
 * Takes the base64 part of a data URL (`data:<type>;base64,<data>`). A value that is not a data URL
 * is returned as it is, so an already stripped value passes through.
 */
export function stripDataUrlPrefix(dataUrl: string): string {
  if (!dataUrl?.startsWith('data:')) {
    return dataUrl ?? '';
  }

  const comma = dataUrl.indexOf(',');
  return comma < 0 ? '' : dataUrl.substring(comma + 1);
}

/**
 * Reads a file the user picked into base64, without the data-URL prefix, ready for an
 * `UploadFileInput`. Emits once and completes; errors when the browser cannot read the file.
 */
export function readFileBase64(file: File | Blob): Observable<string> {
  return new Observable<string>(subscriber => {
    const reader = new FileReader();

    reader.onload = () => {
      subscriber.next(stripDataUrlPrefix(String(reader.result ?? '')));
      subscriber.complete();
    };
    reader.onerror = () => subscriber.error(reader.error);
    reader.readAsDataURL(file);

    return () => {
      if (reader.readyState === FileReader.LOADING) {
        reader.abort();
      }
    };
  });
}

/** Reads a picked file into the `{ fileName, contentBase64 }` shape uploads take. */
export function readUploadFile(file: File): Observable<Base64File> {
  return new Observable<Base64File>(subscriber =>
    readFileBase64(file).subscribe({
      next: contentBase64 => subscriber.next({ fileName: file.name, contentBase64 }),
      error: error => subscriber.error(error),
      complete: () => subscriber.complete(),
    }),
  );
}
