const express = require("express");
const moment = require("moment");
const app = express();

app.use((req, res, next) => {
	console.log(`[${moment().format("YYYY-MM-DD HH:mm:ss")}] ${req.url}`);
	next();
})

app.use(express.static("."));

app.listen(9080, () => {
	console.log(`Server started on port ${9080}`);
});