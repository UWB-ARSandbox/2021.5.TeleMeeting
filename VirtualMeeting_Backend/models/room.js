const db = require('../models/database')

const Room = function (room) {
    this.title = room.title
    this.image = room.image
    this.UUID = room.UUID
    this.description = room.description
    this.creator = room.creator
    this.tag = room.tag
    this.arc_id = room.arc_id
};

Room.create = (newRoom, result) => {
    db.run("INSERT INTO Rooms (uuid, title, image, description, creator, tag, arc_id) VALUES (?, ?, ?, ?, ?, ?, ?)",
        newRoom.UUID, newRoom.title, newRoom.image, newRoom.description,
        newRoom.creator, newRoom.tag, newRoom.arc_id, (err) => {
            if (err) {
                console.error(err)
                result(err, null)
                return
            }

            console.log("Created new room: ", {...newRoom})
            result(null, {...newRoom})
        });
};

Room.getByUUID = (UUID, result) => {
    db.get("SELECT * FROM Rooms where uuid = ?", UUID, (err, res) => {
        if (err) {
            console.error(err)
            result(err, null)
            return
        }

        if (res === undefined) {
            result({kind: "not_found"}, null);
            return;
        }

        result(null, res)
    });
}

Room.searchByTitle = (query, offset, limit, result) => {
    db.all("SELECT * FROM Rooms where title LIKE ? LIMIT ?, ?", query, offset, limit, (err, res) => {
        if (err) {
            console.error(err)
            result(err, null)
            return
        }

        result(null, res)
    });
}

Room.getAllRooms = (offset, limit, result) => {
    db.all("SELECT * FROM Rooms LIMIT ?, ?", offset, limit, (err, res) => {
        if (err) {
            console.error(err)
            result(err, null)
            return
        }

        result(null, res)
    });
}

module.exports = Room;
