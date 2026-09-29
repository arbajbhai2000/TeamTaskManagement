import api from "./api";

const getDashboard = async (filters = {}) => {
  const response = await api.get("/Dashboards", {
    params: {
      status: filters.status || undefined,
      priority: filters.priority || undefined,
      deadline: filters.deadline || undefined,
    },
  });

  return response.data;
};

export default {
  getDashboard,
};