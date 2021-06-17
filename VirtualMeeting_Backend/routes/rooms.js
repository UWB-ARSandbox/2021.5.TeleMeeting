const express = require('express');
const router = express.Router();
const multer  = require('multer')
const storage = multer.memoryStorage()
const upload = multer({ storage: storage })
const Rooms = require('../controllers/rooms.controller')

/* Room object router paths */
const up = upload.fields([{name: 'image', maxCount: 1}, {name: 'room_data', maxCount: 1}])
router.post('/submit', up, Rooms.submit)
router.get('/details/:UUID', Rooms.getDetails)
router.get('/search/:keywords/:page?', Rooms.search)
router.get('/browse/:page?', Rooms.browse)
router.get('/getBlob/:blob_id', Rooms.downloadBlob)

module.exports = router;
