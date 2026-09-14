type UnavailableNoticeProps = {
  feature: string;
};

export default function UnavailableNotice({ feature }: UnavailableNoticeProps) {
  return (
    <p role="status" className="rounded-lg border bg-muted p-4 text-sm text-muted-foreground">
      {feature} is temporarily unavailable.
    </p>
  );
}
