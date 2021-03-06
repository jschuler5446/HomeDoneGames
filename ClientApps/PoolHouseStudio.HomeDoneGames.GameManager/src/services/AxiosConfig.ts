import axios from "axios";

const baseURL =
  process.env.NODE_ENV === "development"
    ? process.env.REACT_APP_API_URL_DEV
    : process.env.REACT_APP_API_URL_PROD;

export const instance = axios.create({
  baseURL: baseURL,
  timeout: 5000
  //   headers: { "X-Custom-Header": "foobar" } "Content-Type": "application/json; charset=utf-8",
});
