<?php

include "./globals.php";
include "./connect.php";

$session_id = 0;
$server_password = htmlspecialchars(GetURLVariable("password", -1, -1, ""));
$server_id = htmlspecialchars( GetURLVariable("id", -1 -1, ""));

// echo "user email: ". $user_email;
// echo "user password: ". $user_password;

//echo $server_password . '<br>';
//echo $server_id . '<br>';
//echo "sql call is performed...";
$query = "SELECT id FROM `servers` WHERE `password` = '$server_password' and `id` = '$server_id' limit 1"; 
//echo $query . "<br>";
$result = mysqli_query($mysqli, $query);

if (mysqli_num_rows($result) > 0) {
    while($row = mysqli_fetch_assoc($result)) {
        session_start();
        $_SESSION["server_id"] = 1;
        $session_id = session_id();
    }
}

echo $session_id;
//header("./game_choice.php");
?>