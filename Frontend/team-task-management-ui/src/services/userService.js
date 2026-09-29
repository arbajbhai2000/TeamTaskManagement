import api from "./api";

const getMyProfile = async () => {
  const response = await api.get("/Users/me");
  return response.data;
};

const updateMyProfile = async (name) => {
  const response = await api.put("/Users/me", {
    name,
  });

  return response.data;
};

const changePassword = async (
  currentPassword,
  newPassword,
  confirmPassword
) => {
  const response = await api.put("/Users/me/password", {
    currentPassword,
    newPassword,
    confirmPassword,
  });

  return response.data;
};

export default {
  getMyProfile,
  updateMyProfile,
  changePassword,
};