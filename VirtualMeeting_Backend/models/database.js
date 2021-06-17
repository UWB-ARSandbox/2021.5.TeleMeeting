const sqlite3 = require('sqlite3').verbose()

const DB_SOURCE = "db.sqlite"

let db = new sqlite3.Database(DB_SOURCE, (err) => {
    if (err) {
        // Cannot open database
        console.error(err.message)
        throw err
    }

    console.log('Connected to the SQLite database')

    // Create our necessary table migrations
    db.run(`CREATE TABLE Rooms (
        uuid text PRIMARY KEY,
        title text CHECK(LENGTH(title) <= 100) NOT NULL,
        image text NOT NULL,
        description text NOT NULL,
        creator text NOT NULL,
        tag INTEGER default '0',
        arc_id text NOT NULL
        )`, (err) => {
        if (err) {
            console.log("Database already migrated")
        }
    });
});

module.exports = db;
