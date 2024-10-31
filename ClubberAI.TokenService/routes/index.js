'use strict';
var express = require('express');
var router = express.Router();
const { connect, keyStores, utils, KeyPair } = require('near-api-js');
const { parseSeedPhrase } = require('near-seed-phrase');

// Configuration for NEAR connection
const nearConfig = {
    networkId: 'testnet',
    keyStore: new keyStores.InMemoryKeyStore(),
    nodeUrl: 'https://rpc.testnet.near.org',
    walletUrl: 'https://wallet.testnet.near.org',
    helperUrl: 'https://helper.testnet.near.org',
    explorerUrl: 'https://explorer.testnet.near.org'
};

// CLUB token contract details
const CLUB_TOKEN_CONTRACT = 'club.tkn.primitives.testnet'; // Replace with actual contract address
const INITIAL_CLUB_AMOUNT = '100000000000000000000'; // 100 CLUB tokens (adjust decimals as needed)

// Add these helper functions after the config section
async function initializeNearConnection() {
    const keyPair = KeyPair.fromString(process.env.PrivateKey);
    await nearConfig.keyStore.setKey('testnet', "clubberai.testnet", keyPair);
    const near = await connect(nearConfig);
    return await near.account("clubberai.testnet");
}

async function ensureStorageRegistered(account, receiverId) {
    try {
        await account.functionCall({
            contractId: CLUB_TOKEN_CONTRACT,
            methodName: 'storage_deposit',
            args: { account_id: receiverId },
            gas: '100000000000000',
            attachedDeposit: utils.format.parseNearAmount('0.00125')
        });
    } catch (e) {
        console.log('Storage registration error (may be already registered):', e);
    }
}

async function transferTokens(account, receiverId, amount) {
    const result = await account.functionCall({
        contractId: CLUB_TOKEN_CONTRACT,
        methodName: 'ft_transfer',
        args: {
            receiver_id: receiverId,
            amount: amount
        },
        gas: '100000000000000',
        attachedDeposit: '1'
    });
    return result;
}

/* Transfer CLUB tokens to new user */
router.get('/transfer-tokens', async function (req, res) {
    try {
        const { receiverId } = req.query;
        if (!receiverId) {
            return res.status(400).json({ error: 'Receiver address is required' });
        }

        const account = await initializeNearConnection();
        
        // Check existing balance
        try {
            const balance = await account.viewFunction({
                contractId: CLUB_TOKEN_CONTRACT,
                methodName: 'ft_balance_of',
                args: { account_id: receiverId }
            });

            if (balance && balance !== '0') {
                return res.status(400).json({
                    error: 'User already has tokens',
                    currentBalance: balance
                });
            }
        } catch (e) {
            console.log('Balance check error:', e);
        }

        await ensureStorageRegistered(account, receiverId);
        const result = await transferTokens(account, receiverId, INITIAL_CLUB_AMOUNT);

        res.json({
            success: true,
            transactionHash: result.transaction.hash,
            amount: INITIAL_CLUB_AMOUNT
        });

    } catch (error) {
        console.error('Token transfer error:', error);
        res.status(500).json({ 
            error: 'Failed to transfer tokens',
            details: error.message
        });
    }
});

/* Add new transfer-one endpoint */
router.get('/transfer-one', async function (req, res) {
    try {
        const { receiverId } = req.query;
        if (!receiverId) {
            return res.status(400).json({ error: 'Receiver address is required' });
        }

        const account = await initializeNearConnection();
        await ensureStorageRegistered(account, receiverId);
        const result = await transferTokens(account, receiverId, INITIAL_CLUB_AMOUNT);

        res.json({
            success: true,
            transactionHash: result.transaction.hash,
            amount: INITIAL_CLUB_AMOUNT
        });

    } catch (error) {
        console.error('Token transfer error:', error);
        res.status(500).json({ 
            error: 'Failed to transfer tokens',
            details: error.message
        });
    }
});

/* Get CLUB token balance for wallet */
router.get('/get-balance', async function (req, res) {
    try {
        // Get wallet address from query params
        const { walletId } = req.query;
        if (!walletId) {
            return res.status(400).json({ error: 'Wallet address is required' });
        }

        // Initialize NEAR connection
        const near = await connect(nearConfig);
        const account = await near.account("clubberai.testnet");
        
        // Get balance
        const balance = await account.viewFunction({
            contractId: CLUB_TOKEN_CONTRACT,
            methodName: 'ft_balance_of',
            args: {
                account_id: walletId
            }
        });

        res.json({
            success: true,
            walletId: walletId,
            balance: balance
        });

    } catch (error) {
        console.error('Balance check error:', error);
        res.status(500).json({ 
            error: 'Failed to get balance',
            details: error.message,
            success: false
        });
    }
});

/* GET home page. */
router.get('/', function (req, res) {
    res.render('index', { title: 'Test' });
});

module.exports = router;
