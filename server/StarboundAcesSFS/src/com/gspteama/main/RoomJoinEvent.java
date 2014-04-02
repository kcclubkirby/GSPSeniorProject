/*
 * To change this template, choose Tools | Templates
 * and open the template in the editor.
 */
package com.gspteama.main;

import com.gspteama.gamedriver.Game;
import com.gspteama.gamedriver.Player;
import com.gspteama.gamedriver.Ship;
import com.smartfoxserver.v2.core.ISFSEvent;
import com.smartfoxserver.v2.core.SFSEventParam;
import com.smartfoxserver.v2.entities.Room;
import com.smartfoxserver.v2.entities.User;
import com.smartfoxserver.v2.exceptions.SFSException;
import com.smartfoxserver.v2.extensions.BaseServerEventHandler;
import java.util.logging.Level;
import java.util.logging.Logger;

/**
 *
 * @author Martin
 */
public class RoomJoinEvent extends BaseServerEventHandler{

    @Override
    public void handleServerEvent(ISFSEvent isfse) throws SFSException {
        Room room = (Room)(isfse.getParameter(SFSEventParam.ROOM));
        User user = (User)(isfse.getParameter(SFSEventParam.USER));
        
        trace("Player joining room: " + room.getId() + "/" + user.getLastJoinedRoom().getId());
        
        if(room.isGame()){
            try {
                Game game = ((StarboundAcesExtension)this.getParentExtension()).getGame(user.getLastJoinedRoom().getId());

                String username = (String)((User)isfse.getParameter(SFSEventParam.USER)).getName();
                int playerid = (int)((User)isfse.getParameter(SFSEventParam.USER)).getId();

                game.AddPlayer(playerid, new Player(username));
                game.AddShip(playerid, new Ship());
            } catch (Exception ex) {
                Logger.getLogger(RoomJoinEvent.class.getName()).log(Level.SEVERE, null, ex);
            }
        }
    }   
}