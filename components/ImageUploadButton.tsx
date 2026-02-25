import { useRef } from "react";
import { Button } from "./ui/button";
import { ImageUp } from "lucide-react";

const ImageUploadButton = () => {
  const inputRef = useRef<HTMLInputElement | null>(null);

  const handleButtonClick = () => {
    inputRef.current?.click();
  };

  const handleFileChange = () => {};

  return (
    <div>
      <Button type="button" onClick={handleButtonClick} size="default">
        <ImageUp />
        Upload Image
      </Button>

      <input
        ref={inputRef}
        type="file"
        accept="image/jpeg,image/png,image/webp"
        multiple
        hidden
        onChange={handleFileChange}
      />
    </div>
  );
};

export default ImageUploadButton;
