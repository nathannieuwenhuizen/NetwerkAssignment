<?php

include "./globals.php";
include "./connect.php";

SetExistingSession();
session_start();

if (!CheckServerID()) {
    echo "0";
    return;
}

$user_id = null;
$user_password = htmlspecialchars( GetURLVariable("password", -1, -1, ""));
$user_email = htmlspecialchars( GetURLVariable("email", -1 -1, ""));

// echo "user email: ". $user_email;
// echo "user password: ". $user_password;

$user_result = 0;
if (filter_var($user_email, FILTER_VALIDATE_EMAIL)) {
    $query = "SELECT id, first_name, last_name FROM `users` WHERE password = '$user_password' and `e-mail` = '$user_email' limit 1"; 
    $result = mysqli_query($mysqli, $query);
    
    if (mysqli_num_rows($result) > 0) {
        while($row = mysqli_fetch_assoc($result)) {
            $user_result = json_encode($row);
            $user_id = $row["id"];
            $_SESSION["user_id"] = $user_id;
        }
    }
}

echo $user_result;
//header("./game_choice.php");
?>