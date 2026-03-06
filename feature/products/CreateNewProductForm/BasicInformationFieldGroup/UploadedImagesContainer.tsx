import {
  Card,
  CardAction,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { UploadedImagePreview } from "./type";
import { Badge } from "lucide-react";
import { Button } from "@/components/ui/button";

const UploadedImagesContainer = ({
  images,
}: {
  images: UploadedImagePreview[];
}) => {
  return (
    <div>
      {images.length > 0 ? (
        // <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-4">
        //   {images.map((image) => (
        //     <div key={image.id} className="rounded-md border p-2">
        //       <img
        //         src={image.previewUrl}
        //         alt={image.fileName}
        //         className="h-24 w-full rounded object-cover"
        //       />
        //       <p className="mt-2 truncate text-xs" title={image.fileName}>
        //         {image.fileName}
        //       </p>
        //       <p className="text-xs text-muted-foreground">
        //         {image.status === "uploading" && "Uploading..."}
        //         {image.status === "done" && "Uploaded"}
        //         {image.status === "error" &&
        //           (image.errorMessage ?? "Upload failed")}
        //       </p>
        //     </div>
        //   ))}
        // </div>

        <div>
          {images.map((img) => {
            return (
              <Card>
                <img src={img.previewUrl} alt={img.fileName} />
                <CardHeader>
                  <CardAction>
                    <Button></Button>
                    <Button></Button>
                  </CardAction>
                  <CardTitle>{img.fileName}</CardTitle>
                  <CardDescription>
                    A practical talk on component APIs, accessibility, and
                    shipping faster.
                  </CardDescription>
                </CardHeader>
              </Card>
            );
          })}
        </div>
      ) : null}
    </div>
  );
};

export default UploadedImagesContainer;
