import { setupWalletSelector } from "@near-wallet-selector/core";
import { setupModal } from "@near-wallet-selector/modal-ui";
import { setupMyNearWallet } from "@near-wallet-selector/my-near-wallet";
import { setupHereWallet } from "@near-wallet-selector/here-wallet";
import { Contract } from "near-api-js";
import * as nearAPI from "near-api-js";
const { connect, keyStores  } = nearAPI;


const myKeyStore = new keyStores.BrowserLocalStorageKeyStore();

const nearConfig = {
  networkId: "testnet",
  keyStore: myKeyStore, // first create a key store
  nodeUrl: "https://rpc.testnet.near.org",
  walletUrl: "https://testnet.mynearwallet.com/",
  helperUrl: "https://helper.testnet.near.org",
  explorerUrl: "https://testnet.nearblocks.io",
};

let selector;
let modal;

export async function initNear() {
  selector = await setupWalletSelector({
    network: "testnet",
    modules: [setupMyNearWallet(), setupHereWallet()],
  });

  modal = setupModal(selector, {
    contractId: "club.tkn.primitives.testnet",
    methodNames: ["ft_balance_of"],
  });
}

export function signIn() {
  modal.show();
}

export async function signOut() {
  const { selectedWalletId } = selector.store.getState();
  const walletModule = await selector.wallet(selectedWalletId);
  if (walletModule) {
    await walletModule.signOut();
  }
}

export function isSignedIn() {
  return selector.isSignedIn();
}

export function getAccountId() {
  const { accounts } = selector.store.getState();
  return accounts.length > 0 ? accounts[0].accountId : null;
}

export async function getTokenBalance() {
  if (!isSignedIn()) {
    throw new Error('User is not signed in');
  }
  
  const nearConnection = await connect(nearConfig);

  const accountId = getAccountId();

  const account = await nearConnection.account(accountId);

  const contract = new Contract(account, 'club.tkn.primitives.testnet', {
    viewMethods: ['ft_balance_of'],
    changeMethods: [],
  });

  return await contract.ft_balance_of({ account_id: accountId });
}
export function cleanupAudioEvents() {
  const audio = document.getElementById('radioPlayer');
  if (!audio) return;

  audio.removeEventListener('waiting', () => {});
  audio.removeEventListener('playing', () => {});
  audio.removeEventListener('pause', () => {});
  audio.removeEventListener('error', () => {});
}
export function setupAudioEvents(dotnetHelper) {
  const audio = document.getElementById('radioPlayer');
  if (!audio) return;

  audio.addEventListener('waiting', () => {
    dotnetHelper.invokeMethodAsync('OnAudioStateChange', 'waiting');
  });

  audio.addEventListener('playing', () => {
    dotnetHelper.invokeMethodAsync('OnAudioStateChange', 'playing');
  });

  audio.addEventListener('pause', () => {
    dotnetHelper.invokeMethodAsync('OnAudioStateChange', 'pause');
  });
}

window.stopAudio = () => {
  const audio = document.getElementById('radioPlayer');
  if (!audio) return;
  
  audio.pause();
  audio.currentTime = 0;  // Reset to beginning
};

window.toggleMute = () => {
  const audio = document.getElementById('radioPlayer');
  if (!audio) return;
  
  audio.muted = !audio.muted;
  return audio.muted;  // Return the new mute state
};

window.resumeAudio = () => {
  const audio = document.getElementById('radioPlayer');
  if (!audio) return;
  
  audio.play();
};
