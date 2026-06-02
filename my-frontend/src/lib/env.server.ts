if (!process.env.BACKEND_URL) {
  const errorMsg = 'Missing required environment variable: BACKEND_URL\n\nPlease check your .env.local file.';
  console.error(errorMsg);
  throw new Error(errorMsg);
}

export const env = {
  backendUrl: process.env.BACKEND_URL || 'http://127.0.0.1:5143',
};
