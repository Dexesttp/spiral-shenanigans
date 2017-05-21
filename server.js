const express = require("express");
const app = express();

app.use((req, res, next) => {
	console.log(`[${req.url}]`);
	next();
})

app.use(express.static("."));

app.listen(9080, () => {
	console.log(`Server started on port ${9080}`);
});