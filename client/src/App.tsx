import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import ListMedia from "./components/ListMedia";
import CreateMovie from "./components/CreateMedia"; // in viitor CreateMedia care are createMovie, createSeries si createEpisode
import "./App.css";

const App: React.FC = () => {
  return (
    <div className="App">
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/media" />} />
          <Route path="/media" element={<ListMedia />} />
          <Route path="/create" element={<CreateMovie />} />
        </Routes>
      </BrowserRouter>{" "}
    </div>
  );
};

export default App;
