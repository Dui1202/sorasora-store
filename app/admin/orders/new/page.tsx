import UnavailableNotice from "@/components/unavailable-notice";

export default function NewOrderPage() {
  return (
    <div className="p-5">
      <UnavailableNotice feature="Order creation" />
    </div>
  );
}
