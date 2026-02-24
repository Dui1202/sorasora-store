export async function GET() {
  return Response.json({
    url: process.env.SUPABASE_URL,
    keyExists: !!process.env.SUPABASE_KEY,
  });
}
