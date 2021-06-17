const fs = require("fs")

const BLOB_STORE_PATH = __dirname + "/blobs/"

const BlobStore = {}

BlobStore.addBlobToStore = (id, data) => {
    fs.writeFile(BLOB_STORE_PATH + id, data, (err) => {
        if (err) {
            console.log(err);
        }
    });
}

BlobStore.getBlobFromStore = (id, callback) => {
    fs.readFile(BLOB_STORE_PATH + id, callback)
}

module.exports = BlobStore
