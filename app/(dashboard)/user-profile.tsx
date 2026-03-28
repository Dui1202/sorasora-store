import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

const UserProfile = () => {
  return (
    <div className="flex flex-row  gap-3">
      <Avatar className="self-center">
        <AvatarImage src="https://github.com/shadcn.png" alt="@shadcn" />
        <AvatarFallback>?</AvatarFallback>
      </Avatar>
      <div className="flex flex-col">
        <p>Welcome Back</p>
        <p>Duy Thanh Nguyen</p>
      </div>
    </div>
  );
};

export default UserProfile;
