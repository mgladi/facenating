class ScoringSystem {
    playerScores: number[];

    constructor(numOfPlayers: number) {
        if (numOfPlayers < 1 || numOfPlayers > 4) {
            throw new Error("Bad number of player input");
        }

        for (let i = 0; i < numOfPlayers; i++)
        {
            this.playerScores[i] = 0;
        }
    }

    AddRoundScore(playerRoundScores : number[]) {
        if (playerRoundScores == null || playerRoundScores.length != this.playerScores.length) {
            throw new Error("Bad player round score inpute");
        }

        for (let player of playerRoundScores) {
            this.playerScores[player] += playerRoundScores[player];
        }
    }

}