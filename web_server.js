var app, express, http;
//express = require('d:/showbot/nodejs/node_modules/express');
express = require('G:/Remote/Projects/trip_site/nodejs/node_modules/express');
app = express();
//http=require('d:/showbot/nodejs/node_modules/http');
//app.use(express.logger('Request: ' + Date.now()));
app.use(express.static('d:/showbot/mmb_webui'));
//app.listen(process.env.PORT || 26262, function()
//{
//	console.log('request hit');
//});
app.listen(26262, '0.0.0.0');
