import React from "react";
import "../App.css";

interface SkeletonLoaderProps {
  type?: "card" | "text" | "image" | "table-row" | "input";
  width?: string;
  height?: string;
  count?: number;
}

const SkeletonLoader: React.FC<SkeletonLoaderProps> = ({
  type = "text",
  width = "100%",
  height = "1rem",
  count = 1,
}) => {
  const renderSkeleton = () => {
    switch (type) {
      case "card":
        return (
          <div className="skeleton-card">
            <div
              className="skeleton-item"
              style={{ width: "100%", aspectRatio: "2/3", marginBottom: "1rem" }}
            />
            <div
              className="skeleton-item"
              style={{ width: "80%", height: "1.5rem", marginBottom: "0.5rem" }}
            />
            <div
              className="skeleton-item"
              style={{ width: "60%", height: "1rem" }}
            />
          </div>
        );
      case "image":
        return (
          <div
            className="skeleton-item"
            style={{ width, height, borderRadius: "8px" }}
          />
        );
      case "table-row":
        return (
          <tr className="skeleton-table-row">
            {[1, 2, 3, 4, 5, 6].map((i) => (
              <td key={i}>
                <div
                  className="skeleton-item"
                  style={{ width: "100%", height: "2rem" }}
                />
              </td>
            ))}
          </tr>
        );
      case "input":
        return (
          <div
            className="skeleton-item"
            style={{
              width,
              height: "2.5rem",
              borderRadius: "6px",
              marginBottom: "1rem",
            }}
          />
        );
      case "text":
      default:
        return (
          <div
            className="skeleton-item"
            style={{ width, height, marginBottom: "0.8rem" }}
          />
        );
    }
  };

  return (
    <>
      {Array.from({ length: count }).map((_, i) => (
        <div key={i}>{renderSkeleton()}</div>
      ))}
    </>
  );
};

export default SkeletonLoader;
