class Player {
    UserID;
    Username;
    PositionX;
    PositionY;
    BombCount = 1;


    constructor(userID, username, positionX, positionY) {
        this.UserID = userID;
        this.Username = username;
        this.PositionX = positionX;
        this.PositionY = positionY;
    }


    RenderPlayer() {
        //debugger;
        const x = (this.PositionX + 0.5) * grid;
        const y = (this.PositionY + 0.5) * grid;

        context.save();
        context.fillStyle = 'white';
        context.beginPath();
        context.arc(x, y, grid * 0.35, 0, 2 * Math.PI);
        context.fill();
    }
}

function Bomb(row, col, owner) {
    this.row = row;
    this.col = col;
    this.radius = grid * 0.4;
    this.owner = owner;
    this.size = 2;
    this.alive = true;
    this.type = types.bomb;

    this.timer = 3000;

    this.update = function (dt) {
        this.timer -= dt;

        if (this.timer <= 0) {
            return blowUpBomb(this);
        }

        const interval = Math.ceil(this.timer / 500);
        if (interval % 2 == 0) {
            this.radius = grid * 0.4;
        }
        else {
            this.radius = grid * 0.5;
        }

    };

    this.render = function () {
        const x = (this.col + 0.5) * grid;
        const y = (this.row + 0.5) * grid;

        //debugger;
        context.fillStyle = 'black';
        context.beginPath();
        context.arc(x, y, this, this.radius, 0, 2 * Math.PI);
        context.fill();

        const fuseY = (this.radius === grid * 0.5 ? grid * 0.15 : 0);
        context.strokeStyle = 'white';
        context.lineWidth = 5;
        context.beginPath();
        context.arc(
            (this.col + 0.75) * grid,
            (this.row + 0.25) * grid - fuseY,
            10, Math.PI, -Math.PI / 2
        );
        context.stroke();
    };
}

function Explosion(row, col, dir, center) {
    this.row = row;
    this.col = col;
    this.dir = dir;
    this.alive = true;

    this.timer = 300;

    this.update = function (dt) {
        this.timer -= dt;

        if (this.timer <= 0) {
            this.alive = false;
        }
    };

    this.render = function () {
        const x = this.col * grid;
        const y = this.row * grid;
        const horizontal = this.dir.col;
        const vertical = this.dir.row;

        context.fillStyle = '#D72B16';
        context.fillRect(x, y, grid, grid);

        context.fillStyle = '#F39642';

        if (center || horizontal)
            context.fillRect(x, y + 6, grid, grid - 12);

        if (center || vertical)
            context.fillRect(x + 6, y, grid - 12, grid);

        context.fillStyle = '#FFE5A8';
        if (center || horizontal)
            context.fillRect(x, y + 12, grid, grid - 24);

        if (center || vertical)
            context.fillRect(x + 12, y, grid - 24, grid);

    };
}

function blowUpBomb(bomb) {
    if (!bomb.alive) return;

    bomb.alive = false;

    template[bomb.row][bomb.col] = null;

    const dirs = [{
        row: -1,
        col: 0
    }, {
        row: 1,
        col: 0
    }, {
        row: 0,
        col: -1
    }, {
        row: 0,
        col: 1
    }];

    dirs.forEach(dir => {
        for (let i = 0; i < bomb.size; i++) {
            // помечаем каждую такую ячейку своими цифрами
            const row = bomb.row + dir.row * i;
            const col = bomb.col + dir.col * i;
            const cell = template[row][col];
            //debugger;
            // останавливаем взыв, если он достиг неразрушаемой стены
            if (cell === types.wall) {
                return;
            }

            entities.push(new Explosion(row, col, dir, i === 0 ? true : false));

            if (i != 0 && template[row][col] == types.softWall)
                ws.send(JSON.stringify(new ModifyBlockMessage(row, col, true)));
            
            template[row][col] = null;


            if (cell === types.bomb) {
                const nextBomb = entities.find((entity) => {
                    return (
                        entity.type === types.bomb &&
                        entity.row === row && entity.col === col
                    );
                });

                blowUpBomb(nextBomb);
            }

            if (cell) {
                return;
            }
        }
    });
}