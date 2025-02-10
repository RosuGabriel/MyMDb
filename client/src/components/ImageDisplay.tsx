import { useEffect, useState } from "react";
import { staticClient } from "../Data";

const ImageDisplay: React.FC<{
  src: string;
  className?: string;
  alt?: string;
  backupImagePath?: string;
  style?: React.CSSProperties;
}> = ({ src: imagePath, className, alt, backupImagePath, style }) => {
  const [imageUrl, setImageUrl] = useState<string | null>(null);

  useEffect(() => {
    let isMounted = true;

    const fetchAndSetImage = async () => {
      try {
        const response = await staticClient.get(imagePath, {
          responseType: "blob",
        });
        const imageObjectUrl = URL.createObjectURL(response.data);

        if (isMounted) {
          setImageUrl(imageObjectUrl);
        }
      } catch (error) {
        console.error("Could not download image:", error);
      }
    };

    fetchAndSetImage();

    return () => {
      isMounted = false;
      if (imageUrl) {
        URL.revokeObjectURL(imageUrl);
      }
    };
  }, [imagePath]);

  return (
    <div>
      {imageUrl ? (
        <img src={imageUrl} className={className} alt={alt} style={style} />
      ) : (
        <img
          src={backupImagePath}
          className={className}
          alt={alt}
          style={style}
        />
      )}
    </div>
  );
};

export default ImageDisplay;
