import { Button } from "@/components/ui/button";
import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Spinner } from "@/components/ui/spinner";
import { useSortable } from "@dnd-kit/react/sortable";
import { CircleCheck, CircleX, Move, Trash2 } from "lucide-react";
import { UploadedImagePreview } from "./type";

const UploadedImagesContainer = ({
  images,
  remove,
}: {
  images: UploadedImagePreview[];
  remove: (id: string) => void;
}) => {
  return (
    <div>
      {images.length > 0 ? (
        <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
          {images.map((img, index) => (
            <ImageCard key={img.id} img={img} index={index} remove={remove} />
          ))}
        </div>
      ) : null}
    </div>
  );
};

const ImageCard = ({
  img,
  index,
  remove,
}: {
  img: UploadedImagePreview;
  index: number;
  remove: (id: string) => void;
}) => {
  const { ref, handleRef } = useSortable({
    id: img.id,
    index,
    group: "uploaded-images",
  });

  return (
    <Card
      key={img.id}
      className="relative mx-auto w-full max-w-sm pt-0"
      ref={ref}
    >
      <img
        src={img.previewUrl}
        alt={img.fileName}
        className="relative z-20 aspect-square object-cover w-full rounded-t-lg"
      />
      <CardHeader>
        <CardAction className="flex gap-3">
          <Button size="icon" variant="outline" type="button" ref={handleRef}>
            <Move />
          </Button>
          <Button
            size="icon"
            variant="outline"
            type="button"
            onClick={() => {
              remove(img.id);
            }}
          >
            <Trash2 />
          </Button>
        </CardAction>
        <CardTitle>{img.fileName}</CardTitle>
        <CardDescription className="flex align-bottom">
          {img.status === "uploading" && <Spinner scale="1.5" />}
          {img.status === "done" && <CircleCheck />}
          {img.status === "error" && (img.errorMessage ?? <CircleX />)}
        </CardDescription>
      </CardHeader>
    </Card>
  );
};

export default UploadedImagesContainer;
