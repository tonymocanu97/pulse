import { API_BASE_URL, ApiError } from '@/lib/api-client';

export type UploadResult = {
  url: string;
  fileName: string;
  sizeBytes: number;
};

export async function uploadFile(file: File, token: string): Promise<UploadResult> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE_URL}/uploads`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}` },
    body: formData,
  });

  if (!response.ok) {
    throw new ApiError(response.status, await response.text());
  }

  return response.json() as Promise<UploadResult>;
}
