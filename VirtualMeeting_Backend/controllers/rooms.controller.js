const Room = require('../models/room')
const BlobStore = require('../blob_store')
const {v4: uuidv4} = require('uuid');

// The maximum limit of entries per page of the API
const PAGE_LIMIT = 10

exports.getDetails = (req, res) => {
    Room.getByUUID(req.params.UUID, (err, data) => {
        if (err) {
            if (err.kind === "not_found") {
                res.status(404).send({
                    message: "Could not find room with id " + req.params.UUID
                });
            } else {
                res.status(500).send({
                    message: "Error retrieving room with id " + req.params.UUID
                });
            }
            return
        }

        res.send({details: data});
    });
}

exports.search = (req, res) => {
    let offset = 0

    if (req.params.page && req.params.page >= 0) {
        offset = PAGE_LIMIT * req.params.page
    }

    // FIXME: This is actually potatoes, we should use full-text search!
    const query = "%" + req.params.keywords.split("+").join("%") + "%"

    Room.searchByTitle(query, offset, PAGE_LIMIT, (err, data) => {
        if (err) {
            res.status(500).send({
                message: "Error retrieving search results"
            });
            return
        }

        let next_page = true

        // Do we have another page available?
        if (data.length < PAGE_LIMIT) {
            next_page = false
        }

        res.send({results: data, hasNextPage: next_page});
    });
}

exports.browse = (req, res) => {
    let offset = 0

    if (req.params.page && req.params.page >= 0) {
        offset = PAGE_LIMIT * req.params.page
    }

    Room.getAllRooms(offset, PAGE_LIMIT, (err, data) => {
        if (err) {
            res.status(500).send({
                message: "Error retrieving search results"
            });
            return
        }

        let next_page = true

        // Do we have another page available?
        if (data.length < PAGE_LIMIT) {
            next_page = false
        }

        res.send({rooms: data, hasNextPage: next_page});
    });
}

exports.downloadBlob = (req, res) => {
    BlobStore.getBlobFromStore(req.params.blob_id, (err, data) => {
        if (err) {
            res.status(500).send({
                message: "Error downloading blob data"
            });
            return
        }

        res.setHeader('Content-Type', 'application/octet-stream');
        res.send(data)
    });
}

exports.submit = (req, res) => {
    if (!req.files['image'] || !req.files['room_data'] ||
        !req.body.title || !req.body.creator || !req.body.description) {
        res.status(500).send({
            message: "Could not find required fields for room submission"
        });
        return
    }

    const image_uuid = uuidv4();
    const room_data_uuid = uuidv4();

    // Add our image and room files to the blob storage backend
    BlobStore.addBlobToStore(image_uuid, req.files['image'][0].buffer)
    BlobStore.addBlobToStore(room_data_uuid, req.files['room_data'][0].buffer)

    const room = new Room({
        UUID: uuidv4(),
        title: req.body.title,
        description: req.body.description,
        image: image_uuid,
        arc_id: room_data_uuid,
        creator: req.body.creator,
        tag: 1 // FIXME: Tag ID is hard-coded here!
    });

    Room.create(room, (err, data) => {
        if (err) {
            res.status(500).send({
                message: "Error adding a new room"
            });
            return
        }

        res.send({result: data});
    });
}
