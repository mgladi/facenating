var ScoringSystem = (function () {
    function ScoringSystem(numOfPlayers) {
        if (numOfPlayers < 1 || numOfPlayers > 4) {
            throw new Error("Bad number of player input");
        }
        for (var i = 0; i < numOfPlayers; i++) {
            this.playerScores[i] = 0;
        }
    }
    ScoringSystem.prototype.AddRoundScore = function (playerRoundScores) {
        if (playerRoundScores == null || playerRoundScores.length != this.playerScores.length) {
            throw new Error("Bad player round score inpute");
        }
        for (var _i = 0, playerRoundScores_1 = playerRoundScores; _i < playerRoundScores_1.length; _i++) {
            var player = playerRoundScores_1[_i];
            this.playerScores[player] += playerRoundScores[player];
        }
    };
    return ScoringSystem;
}());
//# sourceMappingURL=app.js.map