
class ConnectionMessage {
    MessageType = 0;
    Username;
    UserID;


    constructor(username, userID) {
        this.Username = username;
        this.UserID = userID;
    }
}

class DisconnectionMessage {
    MessageType = 1;
    Username;
    UserID;


    constructor(username, userID) {
        this.Username = username;
        this.UserID = userID;
    }
}

class MoveMessage  {
    MessageType = 2;
    Username;
    UserID;
    PositionX;
    PositionY;

    constructor(username, userID, positionX, positionY){
        this.UserID = userID;
        this.Username = username;
        this.PositionX = positionX;
        this.PositionY = positionY;
    }
}

class ModifyBlockMessage {
    MessageType = 3;
    PositionX;
    PositionY;
    IsDelete;

    constructor(positionX, positionY, isDelete){
        this.PositionX = positionX;
        this.PositionY = positionY;
        this.IsDelete = isDelete;
    }
}

class PlaceBombMessage {
    MessageType = 4;
    PositionX;
    PositionY;

    constructor(positionX, positionY){
        this.PositionX = positionX;
        this.PositionY = positionY;
    }

}


const template = [
    ['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w'],
    ['w', , , , , , , , , , , , , , , , , , 'w'],
    ['w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w'],
    ['w', , , , , , , , , , , , , , , , , , 'w'],
    ['w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w'],
    ['w', , , , , , , , , , , , , , , , , , 'w'],
    ['w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w'],
    ['w', , , , , , , , , , , , , , , , , , 'w'],
    ['w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w', , 'w'],
    ['w', , , , , , , , , , , , , , , , , , 'w'], 
    ['w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w', 'w']
];

const types = {
    wall: 'w',
    softWall: 1,
    bomb: 2
};



