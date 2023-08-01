package com.igrium.metadata_server;

import java.util.concurrent.CompletableFuture;

import javafx.application.Application;

public class App {
    public static void main(String[] args) {
        new Server().start(8080);
        Application.launch(MainWindow.class);
    }
}
