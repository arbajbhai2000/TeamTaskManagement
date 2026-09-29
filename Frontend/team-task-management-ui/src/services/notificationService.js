import api from "./api";

const getMyNotifications = async () => {
  const response = await api.get("/Notifications");
  return response.data;
};

const markAsRead = async (notificationId) => {
  await api.put(`/Notifications/${notificationId}/read`);
};

export default {
  getMyNotifications,
  markAsRead,
};