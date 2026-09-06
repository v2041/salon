'use client'
import * as React from 'react';
import BottomNavigation from '@mui/material/BottomNavigation';
import BottomNavigationAction from '@mui/material/BottomNavigationAction';
import AutoStoriesIcon from '@mui/icons-material/AutoStories';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import ContentCutIcon from '@mui/icons-material/ContentCut';
import {Paper} from "@mui/material";
import {CalendarMonth, Settings} from "@mui/icons-material";

interface BottomNavigationBarProps {
    value: number;
    onChange: (newValue: number) => void;
}

export default function BottomNavigationBar({value, onChange}: BottomNavigationBarProps) {
    return (
        <Paper
            elevation={3}
            sx={{
                position: 'fixed',
                display: 'flex',
                bottom: 0,
                left: 0,
                right: 0,
                width: '100%',
                justifyContent: 'center',
                backgroundColor: 'transparent',
                boxShadow: 'none'
            }}>
            <BottomNavigation
                showLabels
                value={value}
                onChange={(event, newValue) => {
                    onChange(newValue);
                }}
                sx={{
                    width: {
                        xs: '100%',
                        sm: '400px'
                    },
                    '& .MuiBottomNavigationAction-root': {
                        minWidth: 0,
                        maxWidth: 'none',
                        flex: 1
                    },
                    boxShadow: 3,
                    borderRadius: {
                        xs: '0px',
                        sm: '16px 16px 0 0'
                    },
                    backgroundColor: 'background.paper'
                }}
            >
                <BottomNavigationAction label="Настройки" icon={<Settings/>}/>
                <BottomNavigationAction label="Услуги" icon={<ContentCutIcon/>}/>
                <BottomNavigationAction label="Записи" icon={<AutoStoriesIcon/>}/>
                <BottomNavigationAction label="Расписание" icon={<CalendarMonth/>}/>
            </BottomNavigation>
        </Paper>
    );
}