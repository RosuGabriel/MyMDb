import React, { useState, useEffect } from "react";
import { IContinueWatching } from "../Data";
import "bootstrap/dist/css/bootstrap.min.css";
import "../App.css";
import ContinueWatchingItem from "./ContinueWatchingItem";
import { fetchContinueWatching } from "../services/ContinueWatchingService";

const ContinueWatching: React.FC = () => {
  const [watchingItems, setWatchingItems] = useState<IContinueWatching[]>([]);

  useEffect(() => {
    const fetchCWList = async () => {
      let fetchedCW: IContinueWatching[] = await fetchContinueWatching();
      setWatchingItems(fetchedCW);
    };

    fetchCWList();
  }, []);

  function handleRemoveItem(id: string) {
    setWatchingItems(watchingItems.filter((item) => item.mediaId !== id));
  }

  return (
    <>
      {watchingItems.length > 0 && (
        <div className="bg-dark mb-4 gradient-border-3 position-relative pt-4 px-3 pb-3 rounded">
          <h2
            className="h2-bg fw-bold position-absolute orange-border-3 rounded"
            style={{
              top: "-15px",
              left: "50%",
              transform: "translateX(-50%)",
              backgroundColor: "#212529",
              padding: "0 15px",
              fontSize: "1.3rem",
              whiteSpace: "nowrap",
            }}
          >
            <span className="gradient-text">Continue Watching</span>
          </h2>

          <div
            className="d-flex overflow-auto pt-2"
            style={{
              gap: "12px",
              whiteSpace: "nowrap",
            }}
          >
            {watchingItems.map((continueWatch) => (
              <ContinueWatchingItem
                key={continueWatch.mediaId}
                cw={continueWatch}
                defaultImage="/film.png"
                onDelete={handleRemoveItem}
              />
            ))}
          </div>
        </div>
      )}
    </>
  );
};

export default ContinueWatching;
