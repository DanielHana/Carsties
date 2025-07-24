import EmptyFilter from "@/app/components/EmptyFilter";

export default function SignIn({
  searchParams,
}: {
  searchParams: { callbackUrl?: string };
}) {
  return (
    <EmptyFilter
      title="You must be logged in to view this page"
      subtitle="Please log in to continue"
      showLogin
      callbackUrl={searchParams.callbackUrl || "/"}
    />
  );
}
