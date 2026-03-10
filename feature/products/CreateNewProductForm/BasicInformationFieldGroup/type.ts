export type UploadedImagePreview = {
  id: string;
  file: File;
  fileName: string;
  previewUrl: string;
  status: UploadStatus;
  errorMessage?: string;
  url?: string;
  position: number;
};

export type UploadImageResponse = {
  path: string;
  publicUrl: string;
};

type UploadStatus = "uploading" | "done" | "error";
