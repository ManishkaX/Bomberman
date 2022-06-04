const canvas = document.getElementById('game');
canvas.width = innerWidth * 0.845;
canvas.height = innerHeight * 0.859;
// содержимое холста
const context = canvas.getContext('2d');
// размеры одной клетки, количество строк и столбцов
const grid = 64;
const numRows = 11;
const numCols = 19;

// создаём кирпичные стены, которые потом расставим по всему полю и будем взрывать. На них будет кирпичный рисунок. Наша задача — нарисовать на стене этот рисунок.
const softWallCanvas = document.createElement('canvas');
const softWallCtx = softWallCanvas.getContext('2d');
// размер квадратика стены равен размеру клетки игрового поля
softWallCanvas.width = softWallCanvas.height = grid;

// цвет швов между кирпичами
softWallCtx.fillStyle = 'black';
// закрашиваем ими всю клетку
softWallCtx.fillRect(0, 0, grid, grid);
// цвет кирпича
softWallCtx.fillStyle = '#a9a9a9';

// первый ряд кирпичей 
softWallCtx.fillRect(1, 1, grid - 2, 20);
// второй ряд кирпичей
softWallCtx.fillRect(0, 23, 20, 18);
softWallCtx.fillRect(22, 23, 42, 18);
// третий ряд кирпичей
softWallCtx.fillRect(0, 43, 42, 20);
softWallCtx.fillRect(44, 43, 20, 20);

// теперь создадим неразрушаемые блоки — их нельзя будет уничтожить
const wallCanvas = document.createElement('canvas');
const wallCtx = wallCanvas.getContext('2d');
// тоже размером с игровую клетку
wallCanvas.width = wallCanvas.height = grid;

// цвет тени
wallCtx.fillStyle = 'black';
wallCtx.fillRect(0, 0, grid, grid);
// цвет верхнего освещения — для объёма
wallCtx.fillStyle = 'white';
wallCtx.fillRect(0, 0, grid - 2, grid - 2);
// цвет стены
wallCtx.fillStyle = '#a9a9a9';
wallCtx.fillRect(2, 2, grid - 4, grid - 4);

let last;
let dt;
let entities = [];


// The port number and hostname of the server.
const port = 11000;
const host = 'localhost';

let players = new Set();

let username = "Player";
let userID = -1;
let currentPlayer;

let ws = new WebSocket("ws://" + host + ':' + port)

ws.onopen = function () {
    //alert("Соединение установлено.");

    ws.send(JSON.stringify(new ConnectionMessage(username, userID)));
};

ws.onmessage = function (event) {


    let messageRaw = JSON.parse(event.data);

    switch (messageRaw.MessageType) {
        case 0:
            console.log(event.data);
            let cMessage = new ConnectionMessage(messageRaw.Username, messageRaw.UserID);

            //  не работает с >1 неинициализированным юзером (или нет) 
            if (userID == -1) {
                userID = cMessage.UserID;

                break;
            }

            players.add(new Player(cMessage.UserID, cMessage.Username, 1, 1));

            console.log(players);

            break;

        case 1:
            console.log("Получены данные " + event.data);


            deletePlayerById(messageRaw.UserID);
            console.log(players);

            break;

        case 2:
            console.log(messageRaw);
            players.forEach(player => {
                if (player.UserID === messageRaw.UserID) {
                    player.PositionX = messageRaw.PositionX;
                    player.PositionY = messageRaw.PositionY;
                }
            });

            break;

        case 3:
            console.log("Block at (" + messageRaw.PositionX + ";" + messageRaw.PositionY + ")");
            if (!messageRaw.IsDelete) {
                template[messageRaw.PositionX][messageRaw.PositionY] = types.softWall;
            }
            else {
                template[messageRaw.PositionX][messageRaw.PositionY] = null;
            }

            drawMap();

            break;

        case 4:

            break;

        case 5:
            //alert("Получены данные " + event.data);
            //console.log(messageRaw.Players.length);
            for (let i = 0; i < messageRaw.Players.length; i++) {
                players.add(new Player(messageRaw.Players[i].UserID, messageRaw.Players[i].Username, messageRaw.Players[i].PositionX, messageRaw.Players[i].PositionY));
            }

            searchPlayerById(userID);
            requestAnimationFrame(gameLoop);

            console.log(currentPlayer);
            console.log(players);
            break;

        default:
            break;
    }
};

ws.onerror = function (error) {
    alert("Ошибка " + error.message);
    ws.close();
};

window.onunload = function () {
    ws.send(JSON.stringify(new DisconnectionMessage(username, userID)));
    ws.close();
}



function drawMap() {
    for (let row = 0; row < numRows; row++) {
        for (let col = 0; col < numCols; col++) {
            switch (template[row][col]) {
                case types.wall:
                    context.drawImage(wallCanvas, col * grid, row * grid);
                    break;
                case types.softWall:
                    context.drawImage(softWallCanvas, col * grid, row * grid);
                    break;
            }
        }
    }
}


function deletePlayerById(UserID) {
    players.forEach((player) => {
        if (player.UserID === UserID) {
            players.delete(player);
        }
    });
}

function searchPlayerById(UserID) {
    players.forEach(player => {
        if (player.UserID === UserID) {
            currentPlayer = player;
        }

    });
}

function gameLoop(timestamp) {
    requestAnimationFrame(gameLoop);

    context.clearRect(0, 0, canvas.width, canvas.height);

    if (!last) {
        last = timestamp;
    }

    drawMap();

    dt = timestamp - last;
    last = timestamp;
    //console.log(currentPlayer);
    //debugger;
    entities.forEach(entity => {
        entity.update(dt);
        entity.render();
    })

    entities = entities.filter(entity => entity.alive);

    players.forEach(player => {
        player.RenderPlayer();
    });

   
}

document.addEventListener('keydown', function (e) {
    let col = currentPlayer.PositionX;
    let row = currentPlayer.PositionY;
    console.log(e.key + " " + row + " " + col);

    switch (e.key) {
        case 'w':
            row--;
            break;

        case 'a':
            col--;
            break;

        case 's':
            row++;
            break;

        case 'd':
            col++;
            break;
        case ' ':
            if (entities.filter(entity => { return entity.type === types.bomb && entity.owner === currentPlayer}).length < currentPlayer.BombCount)
            {
                const bomb = new Bomb(row, col, currentPlayer);

                entities.push(bomb);
                template[row][col] = types.bomb;
                console.log("Bomb at (" + row + ";" + col + ")");
            }

            break;
        default:
            break;
    }
    if (!template[row][col]) {
        currentPlayer.PositionX = col;
        currentPlayer.PositionY = row;

        //debugger;
        ws.send(JSON.stringify(new MoveMessage(currentPlayer.Username, currentPlayer.UserID, currentPlayer.PositionX, currentPlayer.PositionY)));
    }
});



// ws.onclose = function (event) {
//     if (event.wasClean) {
//         alert('Соединение закрыто чисто');
//     } else {
//         alert('Обрыв соединения');
//     }
//     alert('Код: ' + event.code + ' причина: ' + event.reason);

//     ws.send(JSON.stringify(new DisconnectionMessage(username, userID)));
// };
