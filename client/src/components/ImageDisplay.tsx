import { useEffect, useState } from "react";
import { staticClient } from "../Data";

const ImageDisplay: React.FC<{
  src: string;
  className?: string;
  alt?: string;
  backupImagePath?: string;
  style?: React.CSSProperties;
}> = ({ src: imagePath, className, alt, backupImagePath, style }) => {
  const [currentSrc, setCurrentSrc] = useState<string | null>(null);
  const [showTextOverlay, setShowTextOverlay] = useState(false);

  useEffect(() => {
    let isMounted = true;

    const fetchImage = async () => {
      try {
        const response = await staticClient.get(imagePath, {
          responseType: "blob",
        });
        const objectUrl = URL.createObjectURL(response.data);
        if (isMounted) {
          setCurrentSrc(objectUrl);
          setShowTextOverlay(false);
        }
      } catch (error) {
        console.error("Could not download image:", error);
        if (isMounted) {
          setCurrentSrc(backupImagePath || null);
          setShowTextOverlay(true);
        }
      }
    };

    fetchImage();

    return () => {
      isMounted = false;
      if (currentSrc) URL.revokeObjectURL(currentSrc);
    };
  }, [imagePath, backupImagePath]);

  if (!currentSrc) return null;

  return (
    <div
      className={className}
      style={{
        ...style,
        position: "relative",
        display: "inline-block",
        textAlign: "center",
        overflow: "hidden",
      }}
    >
      <img
        src={currentSrc}
        alt={alt}
        style={{
          display: "block",
          width: "100%",
          height: "auto",
          opacity: showTextOverlay ? 0.5 : 1,
        }}
      />
      {showTextOverlay && (
        <div
          style={{
            position: "absolute",
            top: "50%",
            left: "50%",
            transform: "translate(-50%, -50%)",
            fontWeight: "bold",
            color: "#fff",
          }}
        >
          {alt || "Image not available"}
        </div>
      )}
    </div>
  );
};

export default ImageDisplay;
