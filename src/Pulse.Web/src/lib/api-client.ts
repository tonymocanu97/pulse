const API_BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5035/api';
const API_ORIGIN = API_BASE_URL.replace(/\/api\/?$/, '');

export class ApiError extends Error {
  status: number;

  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

type ApiFetchOptions = Omit<RequestInit, 'body'> & {
  body?: unknown;
  token?: string | null;
};

async function extractErrorMessage(response: Response): Promise<string> {
  const raw = await response.text();

  try {
    const parsed = JSON.parse(raw);
    if (typeof parsed === 'string') return parsed;
    if (parsed?.errors) return Object.values<string[]>(parsed.errors).flat().join(' ');
    if (parsed?.detail) return parsed.detail;
    if (parsed?.title) return parsed.title;
  } catch {
    // Body wasn't JSON (plain text error) - fall through to the raw text.
  }

  return raw || `Request failed with status ${response.status}`;
}

export async function apiFetch<T>(path: string, options: ApiFetchOptions = {}): Promise<T> {
  const { body, token, headers, ...rest } = options;

  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...rest,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...headers,
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    throw new ApiError(response.status, await extractErrorMessage(response));
  }

  // Check the actual body rather than trusting status code conventions -
  // both 200 and 204 responses can come back with no content, and calling
  // .json() on an empty body throws a parse error.
  const raw = await response.text();
  if (!raw) {
    return undefined as T;
  }

  return JSON.parse(raw) as T;
}

// Most media URLs are relative paths served by the backend (e.g. /uploads/xxx from a
// real upload), so they need the API origin prefixed. Demo/seed assets instead live in
// the frontend's own public/ folder and come back as absolute URLs - those are already
// complete and must be used as-is, or prefixing would mangle them.
function resolveAssetUrl(url: string): string {
  return /^https?:\/\//.test(url) ? url : `${API_ORIGIN}${url}`;
}

export { API_BASE_URL, API_ORIGIN, resolveAssetUrl };
