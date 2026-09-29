import api from "./api";

const getByTaskId = async (taskId) => {
  const response = await api.get(`/Comments/task/${taskId}`);
  return response.data;
};

const create = async (content, taskItemId) => {
  const response = await api.post("/Comments", {
    content,
    taskItemId,
  });

  return response.data;
};

const remove = async (commentId) => {
  await api.delete(`/Comments/${commentId}`);
};

export default {
  getByTaskId,
  create,
  remove,
};