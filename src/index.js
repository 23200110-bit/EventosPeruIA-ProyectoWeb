const express = require('express');
const bodyParser = require('body-parser');
const ordersRouter = require('./orders');
const ticketsRouter = require('./tickets');

const app = express();
app.use(bodyParser.json());

app.use('/orders', ordersRouter);
app.use('/tickets', ticketsRouter);

app.get('/health', (req, res) => res.json({ status: 'ok' }));

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  // eslint-disable-next-line no-console
  console.log(`Server running on port ${PORT}`);
});

module.exports = app;
