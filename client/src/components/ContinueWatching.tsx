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
        <div className="bg-dark p-1 rounded mb-4 border border-warning border-3">
          <h2 className="text-secondary mb-1">Continue Watching</h2>
          <div
            className="d-flex overflow-auto"
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
