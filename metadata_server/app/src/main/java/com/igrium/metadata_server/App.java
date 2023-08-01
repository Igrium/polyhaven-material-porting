package com.igrium.metadata_server;

import com.igrium.metadata_server.ui.AppUI;

import javafx.application.Application;

public class App {
    public static void main(String[] args) {
        new Server().start(8080);
        Application.launch(AppUI.class);
    }
}
