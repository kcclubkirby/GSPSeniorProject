/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */

package com.gspteama.gamedriver;

/**
 *
 * @author Martin
 */
public class EnergyPowerup implements IPowerup{

    @Override
    public void execute(Ship ship) {
        ship.restoreEnergy(1000000.00f);
    }
    
}
