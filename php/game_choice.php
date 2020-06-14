<?php

include "./globals.php";

$game_id = GetURLVariable("id", 0, -1, 0);

session_start();
$_SESSION["game_id"] = $game_id; 

echo "user id from session = " . $_SESSION["user_id"];
echo "game id from session = " . $_SESSION["game_id"];

//header("./game_choice.php");
?>