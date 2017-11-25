const express = require('express');
const app = express();
app.use(function (req, res, next) {
  res.setHeader('Access-Control-Allow-Origin', '*');
  next();
});

app.use(express.static('.'));

app.listen(1337, function () {
  console.log('Example app listening on port 1337!');
})
