import { BrowserRouter, Routes, Route } from "react-router-dom";
import Login from "../pages/Login";
import Register from "../pages/Register";
import Dashboard from "../pages/Dashboard";
import Tasks from "../pages/Tasks";
import TaskDetails from "../pages/TaskDetails";
import Comments from "../pages/Comments";
import Notifications from "../pages/Notifications";
import Settings from "../pages/Settings";
import Teams from "../pages/Teams";
import CreateTask from "../pages/CreateTask";
import Users from "../pages/Users";

function AppRoutes() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<h1>Team Task Management</h1>} />

        <Route path="/login" element={<Login />} />

        <Route path="/register" element={<Register />} />

        <Route path="/dashboard" element={<Dashboard />} />

         <Route path="/tasks" element={<Tasks />} />

        <Route path="/teams" element={<Teams />} />

        <Route path="/tasks/create" element={<CreateTask />}/>
          
        <Route path="/tasks/:id" element={<TaskDetails />} />

         <Route path="/comments" element={<Comments />} />

         <Route path="/Notifications" element={<Notifications />} />

         <Route path="/settings" element={<Settings />} />

         <Route path="/users" element={<Users />} />

      </Routes>
    </BrowserRouter>
  );
}

export default AppRoutes;