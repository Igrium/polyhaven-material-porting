package com.igrium.metadata_server.ui;

import com.igrium.metadata_server.Server;

import javafx.application.Application;
import javafx.application.Platform;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.stage.Stage;

public class AppUI extends Application {

    private static AppUI instance;

    public static AppUI getInstance() {
        return instance;
    }

    private MainWindow mainWindow;

    public MainWindow getMainWindow() {
        return mainWindow;
    }

    @Override
    public void start(Stage primaryStage) throws Exception {
        instance = this;
        FXMLLoader loader = new FXMLLoader(getClass().getResource("/ui/app.fxml"));

        Parent window = loader.load();
        mainWindow = loader.getController();

        Scene scene = new Scene(window);
        primaryStage.setScene(scene);
        primaryStage.setTitle("Edit Queue");
        primaryStage.show();

        primaryStage.setOnCloseRequest(e -> {
            Platform.exit();
            Server.getInstance().getApp().close();
        });
    }

}
