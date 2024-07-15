import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import ListMedia from "./components/ListMedia";
import { CreateMedia, AddEpisode } from "./components/CreateMedia";
import Register from "./components/Register";
import Login from "./components/Login";
import ShowMedia from "./components/ShowMedia";
import Navbar from "./components/Navbar";
import Profile from "./components/Profile";
import "bootstrap/dist/css/bootstrap.min.css";
import "./App.css";

const App: React.FC = () => {
  return (
    <div className="App">
      <Navbar />
      <br />
      <div className="content p-5">
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Navigate to="/media" />} />
            <Route path="/media" element={<ListMedia />} />
            <Route path="/media/:id" element={<ShowMedia />} />
            <Route path="/create" element={<CreateMedia />} />
            <Route path="/add-episode/:id" element={<AddEpisode />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="*" element={<Navigate to="/media" />} />
          </Routes>
        </BrowserRouter>{" "}
      </div>
    </div>
  );
};

export default App;
