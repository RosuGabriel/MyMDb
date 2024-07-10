import React from "react";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import ListMedia from "./components/ListMedia";
import CreateMovie from "./components/CreateMedia";
import Register from "./components/Register";
import Login from "./components/Login";
import ShowMedia from "./components/ShowMedia";
import Navbar from "./components/Navbar";
import "bootstrap/dist/css/bootstrap.min.css";
import "./App.css";

const App: React.FC = () => {
  return (
    <div className="App">
      <Navbar />
      <div className="content p-3">
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Navigate to="/media" />} />
            <Route path="/media" element={<ListMedia />} />
            <Route path="/create" element={<CreateMovie />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/media/:id" element={<ShowMedia />} />
            <Route path="*" element={<Navigate to="/media" />} />
          </Routes>
        </BrowserRouter>{" "}
      </div>
    </div>
  );
};

export default App;
