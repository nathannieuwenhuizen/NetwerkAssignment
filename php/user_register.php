<?php

include "./globals.php";
include "./connect.php";

$first_name = ( GetURLVariable("first_name", -1, -1, ""));
$last_name = ( GetURLVariable("last_name", -1, -1, ""));
$user_password = ( GetURLVariable("password", -1, -1, ""));
$user_email = ( GetURLVariable("email", -1 -1, ""));


$query = "SELECT id FROM `users` WHERE `e-mail` = '$user_email'"; 
$result = mysqli_query($mysqli, $query);

// echo $user_email;
// echo (mysqli_num_rows($result) > 0) . "<br>";
if (mysqli_num_rows($result) > 0) {
    echo "0";
}
else {
        $query = "INSERT INTO `users` (`id`, `first_name`, `last_name`, `e-mail`, `password`, `date`) VALUES (NULL, '$first_name', '$last_name', '$user_email', '$user_password', CURRENT_TIMESTAMP);"; 
        if (!($result = $mysqli->query($query))) {
            echo "0";
            showerror($mysqli->errno,$mysqli->error);
        } else {
            echo "1";
        }
}
?>